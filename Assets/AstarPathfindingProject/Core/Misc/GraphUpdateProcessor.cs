using System.Collections.Generic;
using System.Threading;
using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	using UnityEngine.Assertions;

#if NETFX_CORE
	using Thread = Pathfinding.WindowsStore.Thread;
#else
	using Thread = System.Threading.Thread;
#endif

	class GraphUpdateProcessor {
		public event System.Action OnGraphsUpdated;

		/** Holds graphs that can be updated */
		readonly AstarPath astar;

#if !UNITY_WEBGL
		/**
		 * Reference to the thread which handles async graph updates.
		 * \see ProcessGraphUpdatesAsync
		 */
		Thread graphUpdateThread;
#endif

		/** Used for IsAnyGraphUpdateInProgress */
		bool anyGraphUpdateInProgress;

#if UNITY_2017_3_OR_NEWER && !UNITY_WEBGL
		CustomSampler asyncUpdateProfilingSampler;
#endif

		/**
		 * Queue containing all waiting graph update queries. Add to this queue by using \link AddToQueue \endlink.
		 * \see AddToQueue
		 */
		readonly Queue<GraphUpdateObject> graphUpdateQueue = new Queue<GraphUpdateObject>();

		/** Queue of all async graph updates waiting to be executed */
		readonly Queue<GUOSingle> graphUpdateQueueAsync = new Queue<GUOSingle>();

		/** Queue of all non-async graph update post events waiting to be executed */
		readonly Queue<GUOSingle> graphUpdateQueuePost = new Queue<GUOSingle>();

		/** Queue of all non-async graph updates waiting to be executed */
		readonly Queue<GUOSingle> graphUpdateQueueRegular = new Queue<GUOSingle>();

		readonly System.Threading.ManualResetEvent asyncGraphUpdatesComplete = new System.Threading.ManualResetEvent(true);

#if !UNITY_WEBGL
		readonly System.Threading.AutoResetEvent graphUpdateAsyncEvent = new System.Threading.AutoResetEvent(false);
		readonly System.Threading.AutoResetEvent exitAsyncThread = new System.Threading.AutoResetEvent(false);
#endif

		/** Returns if any graph updates are waiting to be applied */
		public bool IsAnyGraphUpdateQueued { get { return graphUpdateQueue.Count > 0; } }

		/** Returns if any graph updates are in progress */
		public bool IsAnyGraphUpdateInProgress { get { return anyGraphUpdateInProgress; } }

		/** Order type for updating graphs */
		enum GraphUpdateOrder {
			GraphUpdate,
			// FloodFill
		}

		/** Holds a single update that needs to be performed on a graph */
		struct GUOSingle {
			public GraphUpdateOrder order;
			public IUpdatableGraph graph;
			public GraphUpdateObject obj;
		}

		public GraphUpdateProcessor (AstarPath astar) {
			this.astar = astar;
		}

		/** Work item which can be used to apply all queued updates */
		public AstarWorkItem GetWorkItem () {
			return new AstarWorkItem(QueueGraphUpdatesInternal, ProcessGraphUpdates);
		}

		public void EnableMultithreading () {
#if !UNITY_WEBGL
			if (graphUpdateThread == null || !graphUpdateThread.IsAlive) {
#if UNITY_2017_3_OR_NEWER && !UNITY_WEBGL
				asyncUpdateProfilingSampler = CustomSampler.Create("Graph Update");
#endif

				graphUpdateThread = new Thread(ProcessGraphUpdatesAsync);
				graphUpdateThread.IsBackground = true;

				// Set the thread priority for graph updates
				// Unless compiling for windows store or windows phone which does not support it
#if !UNITY_WINRT
				graphUpdateThread.Priority = System.Threading.ThreadPriority.Lowest;
#endif
				graphUpdateThread.Start();
			}
#endif
		}

		public void DisableMultithreading () {
#if !UNITY_WEBGL
			if (graphUpdateThread != null && graphUpdateThread.IsAlive) {
				// Resume graph update thread, will cause it to terminate
				exitAsyncThread.Set();

				if (!graphUpdateThread.Join(5*1000)) {
					Debug.LogError("Graph update thread did not exit in 5 seconds");
				}

				graphUpdateThread = null;
			}
#endif
		}

		/** Update all graphs using the GraphUpdateObject.
		 * This can be used to, e.g make all nodes in an area unwalkable, or set them to a higher penalty.
		 * The graphs will be updated as soon as possible (with respect to AstarPath.batchGraphUpdates)
		 *
		 * \see FlushGraphUpdates
		 */
		public void AddToQueue (GraphUpdateObject ob) {
			// Put the GUO in the queue
			graphUpdateQueue.Enqueue(ob);
		}

		/** Schedules graph updates internally */
		void QueueGraphUpdatesInternal () {
			while (graphUpdateQueue.Count > 0) {
				GraphUpdateObject ob = graphUpdateQueue.Dequeue();

				foreach (IUpdatableGraph g in astar.data.GetUpdateableGraphs()) {
					NavGraph gr = g as NavGraph;
					if (ob.nnConstraint == null || ob.nnConstraint.SuitableGraph(astar.data.GetGraphIndex(gr), gr)) {
						var guo = new GUOSingle();
						guo.order = GraphUpdateOrder.GraphUpdate;
						guo.obj = ob;
						guo.graph = g;
						graphUpdateQueueRegular.Enqueue(guo);
					}
				}
			}

			GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
			anyGraphUpdateInProgress = true;
		}

		/** Updates graphs.
		 * Will do some graph updates, possibly signal another thread to do them.
		 * Will only process graph updates added by QueueGraphUpdatesInternal
		 *
		 * \param force If true, all graph updates will be processed before this function returns. The return value
		 * will be True.
		 *
		 * \returns True if all graph updates have been done and pathfinding (or other tasks) may resume.
		 * False if there are still graph updates being processed or waiting in the queue.
		 */
		bool ProcessGraphUpdates (bool force) {
			Assert.IsTrue(anyGraphUpdateInProgress);

			if (force) {
				asyncGraphUpdatesComplete.WaitOne();
			} else {
				#if !UNITY_WEBGL
				if (!asyncGraphUpdatesComplete.WaitOne(0)) {
					return false;
				}
				#endif
			}

			Assert.AreEqual(graphUpdateQueueAsync.Count, 0, "Queue should be empty at this stage");

			ProcessPostUpdates();
			if (!ProcessRegularUpdates(force)) {
				return false;
			}

			GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
			if (OnGraphsUpdated != null) OnGraphsUpdated();

			Assert.AreEqual(graphUpdateQueueRegular.Count, 0, "QueueRegular should be empty at this stage");
			Assert.AreEqual(graphUpdateQueueAsync.Count, 0, "QueueAsync should be empty at this stage");
			Assert.AreEqual(graphUpdateQueuePost.Count, 0, "QueuePost should be empty at this stage");

			anyGraphUpdateInProgress = false;
			return true;
		}

		bool ProcessRegularUpdates (bool force) {
			while (graphUpdateQueueRegular.Count > 0) {
				GUOSingle s = graphUpdateQueueRegular.Peek();

				GraphUpdateThreading threading = s.graph.CanUpdateAsync(s.obj);

#if UNITY_WEBGL
				// Never use multithreading in WebGL
				threading &= ~GraphUpdateThreading.SeparateThread;
#else
				// When not playing or when not using a graph update thread (or if it has crashed), everything runs in the Unity thread
				if (force || !Application.isPlaying || graphUpdateThread == null || !graphUpdateThread.IsAlive) {
					// Remove the SeparateThread flag
					threading &= ~GraphUpdateThreading.SeparateThread;
				}
#endif

				if ((threading & GraphUpdateThreading.UnityInit) != 0) {
					// Process async graph updates first.
					// Next call to this function will process this object so it is not dequeued now
					if (StartAsyncUpdatesIfQueued()) {
						return false;
					}

					s.graph.UpdateAreaInit(s.obj);
				}

				if ((threading & GraphUpdateThreading.SeparateThread) != 0) {
					// Move GUO to async queue to be updated by another thread
					graphUpdateQueueRegular.Dequeue();
					graphUpdateQueueAsync.Enqueue(s);

					// Don't start any more async graph updates because this update
					// requires a Unity thread function to run after it has been completed
					// but before the next update is started
					if ((threading & GraphUpdateThreading.UnityPost) != 0) {
						if (StartAsyncUpdatesIfQueued()) {
							return false;
						}
					}
				} else {
					// Unity Thread

					if (StartAsyncUpdatesIfQueued()) {
						return false;
					}

					graphUpdateQueueRegular.Dequeue();

					try {
						s.graph.UpdateArea(s.obj);
					} catch (System.Exception e) {
						Debug.LogError("Error while updating graphs\n"+e);
					}

					if ((threading & GraphUpdateThreading.UnityPost) != 0) {
						s.graph.UpdateAreaPost(s.obj);
					}
				}
			}

			if (StartAsyncUpdatesIfQueued()) {
				return false;
			}

			return true;
		}

		/** Signal the graph update thread to start processing graph updates if there are any in the #graphUpdateQueueAsync queue.
		 * \returns True if the other thread was signaled.
		 */
		bool StartAsyncUpdatesIfQueued () {
			if (graphUpdateQueueAsync.Count > 0) {
#if UNITY_WEBGL
				throw new System.Exception("This should not happen in WebGL");
#else
				asyncGraphUpdatesComplete.Reset();
				graphUpdateAsyncEvent.Set();
				return true;
#endif
			}
			return false;
		}

		void ProcessPostUpdates () {
			while (graphUpdateQueuePost.Count > 0) {
				GUOSingle s = graphUpdateQueuePost.Dequeue();

				GraphUpdateThreading threading = s.graph.CanUpdateAsync(s.obj);

				if ((threading & GraphUpdateThreading.UnityPost) != 0) {
					try {
						s.graph.UpdateAreaPost(s.obj);
					} catch (System.Exception e) {
						Debug.LogError("Error while updating graphs (post step)\n"+e);
					}
				}
			}
		}

	#if !UNITY_WEBGL
		/** Graph update thread.
		 * Async graph updates will be executed by this method in another thread.
		 */
		void ProcessGraphUpdatesAsync () {
			var handles = new [] { graphUpdateAsyncEvent, exitAsyncThread };

			while (true) {
				// Wait for the next batch or exit event
				var handleIndex = WaitHandle.WaitAny(handles);

				if (handleIndex == 1) {
					// Exit even was fired
					// Abort thread and clear queue
					graphUpdateQueueAsync.Clear();
					asyncGraphUpdatesComplete.Set();
					return;
				}

				while (graphUpdateQueueAsync.Count > 0) {
					// Note that no locking is required here because the main thread
					// cannot access it until asyncGraphUpdatesComplete is signaled
					GUOSingle aguo = graphUpdateQueueAsync.Dequeue();

					try {
						if (aguo.order == GraphUpdateOrder.GraphUpdate) {
							aguo.graph.UpdateArea(aguo.obj);
							graphUpdateQueuePost.Enqueue(aguo);
						} else {
							throw new System.NotSupportedException("" + aguo.order);
						}
					} catch (System.Exception e) {
						Debug.LogError("Exception while updating graphs:\n"+e);
					}
				}

				// Done
				asyncGraphUpdatesComplete.Set();
			}
		}
#endif
	}
}