using UnityEngine;

namespace GOAP.Action
{
	public delegate void OnActionReleaseDelegate(UnitModel unit);
	public delegate void OnActionCompleteDelegate(UnitModel unit, bool isCanceled);

	public abstract class BaseAction<T> : ObjectsPool<T>, IAction where T : ObjectsPool<T>, new()
	{

		public event OnActionCompleteDelegate onActionComplete;
#pragma warning restore 67
		protected Condition.ICondition completeCondition;
		protected Precondition.IPrecondition precondition;
		protected UnitModel unit;
		private bool alwaysAskPrecondition;//TODO this must be true for all actions
#if DEBUG_MODE
		private string debugString = "";
		protected string creationStack;
#endif
#if LOOP_TRACKING
		private string createStackTrace;
		public string CreateStackTrace { get { return createStackTrace; } }
#endif

#if DEBUG_MODE
		public string DebugString
		{
			get
			{
				return debugString;
			}
			set 
			{
				debugString = value;
			}
		}

		private bool firstRun = true;

		protected virtual string AdditionalActionData()
		{
			return "";
		}
#endif

		protected void InitAction(Precondition.IPrecondition precondition, Condition.ICondition completeCondition, UnitModel unit)
		{
			InitAction(precondition, completeCondition, unit, false);
		}

		protected void InitAction(Precondition.IPrecondition precondition, Condition.ICondition completeCondition, UnitModel unit, bool alwaysAskPrecondition)
		{
			this.completeCondition = completeCondition;
			this.precondition = precondition;
			this.unit = unit;
			this.alwaysAskPrecondition = alwaysAskPrecondition;

			if (completeCondition != null && completeCondition.IsComplete())
			{
#if UNITY_EDITOR
				Debug.LogWarning(this + "  <color=white>Precondition returns completed condition: </color>" + completeCondition + " / " + completeCondition.IsComplete() + " / " + unit + " / " + (unit != null ? "" + unit.MostVisibleEnemy : " no unit"));
#else
				Debug.LogError(this + " - Precondition returns completed condition");
#endif
			}
#if DEBUG_MODE
			if (PlayerPreferences.instance.enableStackTraces)
			{
				creationStack = "Precondition = " + precondition + " Condition = " + completeCondition + " Unit " + unit + "\n";
				creationStack += System.Environment.StackTrace;
			}
#endif

#if LOOP_TRACKING
			createStackTrace = new System.Diagnostics.StackTrace(true).ToString();
			/*if (completeCondition != null && completeCondition.IsComplete())
			{
				Debug.Log("starting with completed condition");
			}*/
#endif
			/*if (unit != null && unit.data.name == "Moody")
			{
				Debug.Log("Start " + ToString() + " " + unit.data.name);
			}*/
		}

		public bool AlwaysAskPrecondition { get { return alwaysAskPrecondition; } }

		protected abstract float Process(float dt);

		protected virtual void OnComplete(bool isCanceled)
		{
			/*if (unit != null && unit.data.name == "Moody")
			{
				Debug.Log("Finish " + ToString() + " " + unit.data.name);
			}*/
			if (onActionComplete != null)
			{
				onActionComplete(unit, isCanceled);
			}
			onActionComplete = null;
			
			Release();
		}

		public void SafeRelease()
		{
			if (precondition != null)
			{
				precondition.Release();
				precondition = null;
			}
			if (completeCondition != null)
			{
				completeCondition.Release();
				completeCondition = null;
			}

			if (onActionComplete != null)
			{
				//Debug.LogError("Incorrect action canceling for unit " + unit + " / " + this + " / " + onActionComplete + " / " + precondition + " / " + completeCondition);
				//onActionComplete(unit, true);
				onActionComplete = null;
			}

			unit = null;

			base.Release();
		}
		
		public override void Release()
		{
			if (onActionComplete != null)
			{
				Debug.LogError("Incorrect action canceling for unit " + unit + " / " + this + " / " + onActionComplete + " / " + precondition + " / " + completeCondition);
			}
			SafeRelease();
		}

		public bool CheckCompleteCondition
		{
			get
			{
				if (completeCondition == null)
				{
					return false;
				}
				return completeCondition.IsComplete();
			}
		}

		public float Run(float dt)
		{
			if (completeCondition != null && completeCondition.IsComplete())
			{
#if DEBUG_MODE
				if (firstRun && PlayerPreferences.instance.enableLoopedActionsFinder)
				{
					Debug.LogError("Action :" + GetType().Name + " has exited at the first run, data: " + AdditionalActionData());
				}
#endif
				OnComplete(true);
				return dt;
			}

			var result = Process(dt);
			if (result > 0f)
			{
				OnComplete(false);
			}

#if DEBUG_MODE
			firstRun = false;
#endif

			return result;
		}


		public IAction PreconditionAction
		{
			get { return precondition != null ? precondition.NextAction : null; }
		}

		public virtual void Cancel()
		{
			OnComplete(true);
		}

		public float Priority { get; set; }

		public override string ToString()
		{
			return 
#if DEBUG_MODE
				(string.IsNullOrEmpty(DebugString) ? GetType().Name : DebugString) +
#else
				GetType().Name +
#endif
				" / cond = " + completeCondition + " / precond = " + precondition 
#if UNITY_EDITOR				
				+ " unit = " + unit
#endif				
				;
		}
	}
}