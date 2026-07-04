using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cute;

public class LeanThreadPool
{
	private static LeanThreadPool _instance;

	private Thread[] _threads;

	private Semaphore _semaphore;

	private object _jobsLock;

	private object _convergeLock;

	private List<ParallelJob> _jobs;

	private bool _quit;

	private int _convergeCount;

	public static LeanThreadPool Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new LeanThreadPool();
			}
			return _instance;
		}
	}

	private LeanThreadPool()
	{
		int processorCount = SystemInfo.processorCount;
		_jobs = new List<ParallelJob>();
		_jobsLock = new object();
		_convergeLock = new object();
		_semaphore = new Semaphore(0, int.MaxValue);
		_threads = new Thread[processorCount];
		for (int i = 0; i < _threads.Length; i++)
		{
			_threads[i] = new Thread(ThreadFunction);
			_threads[i].Start();
		}
	}

	private void ThreadFunction()
	{
		ParallelJob parallelJob = null;
		while (!_quit)
		{
			_semaphore.WaitOne();
			lock (_jobsLock)
			{
				if (_jobs.Count > 0)
				{
					parallelJob = _jobs[0];
					_jobs.Remove(parallelJob);
				}
			}
			if (parallelJob != null)
			{
				parallelJob.Run();
				parallelJob = null;
			}
		}
		lock (_convergeLock)
		{
			_convergeCount++;
		}
	}

	public void AddJob(ParallelJob job)
	{
		lock (_jobsLock)
		{
			_jobs.Add(job);
		}
		_semaphore.Release();
	}
}
