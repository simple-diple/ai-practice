using UnityEngine;

public class Planner
{
	
    public void ManualUpdate(float dt)
    {
	    var units = MapModel.AllUnits;
	    var unitsCount = units.Count;
	    
	    for (var unitIndex = 0; unitIndex < unitsCount; ++unitIndex)
	    {
		    var unit = units[unitIndex];

		    var actions = unit.Actions;
		    var leftTime = dt;

		    for (var i = 0; i < actions.Count; i++)
		    {
			    if (actions[i].CheckCompleteCondition)
			    {
				    //Debug.Log(unit + " <color=white> STOP ACTION </color> " + actions[i]);
				    for (var c = actions.Count - 1; c >= i; c--)
				    {
					    actions[c].Cancel();
				    }

				    actions.RemoveRange(i, actions.Count - i);
				    if (i == 0)
				    {
					    unit.Idle(true);
				    }

				    break;
			    }
		    }

		    if (actions.Count > 0)
		    {
			    var preconditionAction = actions[actions.Count - 1].PreconditionAction;
			    while (preconditionAction != null)
			    {
				    actions.Add(preconditionAction);
#if DEBUG_MODE
						if (PlayerPreferences.instance.enableLoopedActionsFinder)
						{
							actionsFinder.AddActionByObject(unit, preconditionAction.GetType(), Time.realtimeSinceStartup);
						}
#endif
				    preconditionAction = preconditionAction.PreconditionAction;
			    }
		    }

//#if LOOP_TRACKING
		    var justAdded = false;
//#endif
		    var loopsCount = 0;
		    while (leftTime > 0 && loopsCount++ < 20)
		    {
			    if (actions.Count == 0)
			    {
				    unit.Idle(true);
				    break;
			    }

//#if LOOP_TRACKING
			    var startLoopTime = leftTime;
//#endif
			    var action = actions[actions.Count - 1];
			    leftTime = action.Run(leftTime);
			    if (justAdded)
			    {
				    justAdded = false;
				    if (leftTime >= startLoopTime)
				    {
#if LOOP_TRACKING
							Debug.LogWarning("Action " + actions[actions.Count - 1] + " for unit " + unit.name + " was added and removed in 1 frame\n" + actions[actions.Count - 1].CreateStackTrace);
#endif
					    leftTime = 0;
					    actions.RemoveAt(actions.Count - 1);
				    }
			    }

			    if (leftTime > 0)
			    {
				    actions.RemoveAt(actions.Count - 1);
				    if (actions.Count > 0)
				    {
					    var lastAction = actions[actions.Count - 1];
					    if (lastAction.AlwaysAskPrecondition && !lastAction.CheckCompleteCondition)
					    {
						    var preconditionAction = lastAction.PreconditionAction;
						    while (preconditionAction != null)
						    {
							    actions.Add(preconditionAction);
//#if LOOP_TRACKING
							    justAdded = true;
//#endif
#if DEBUG_MODE
									if (PlayerPreferences.instance.enableLoopedActionsFinder)
									{
										actionsFinder.AddActionByObject(unit, preconditionAction.GetType(), Time.realtimeSinceStartup);
									}
#endif
							    preconditionAction = preconditionAction.PreconditionAction;
						    }
					    }
				    }
			    }
		    }

		    if (leftTime > 0)
		    {
			    if (!unit.lastFrameLoopedAction)
			    {
				    var dbgMsg = "Looped action: leftTime = " + leftTime + " / loopsCount = " + loopsCount;
				    for (var ai = 0; ai < actions.Count; ai++)
				    {
					    dbgMsg += "\n" + ai + "Action " + actions[ai];
				    }

				    Debug.LogWarning(dbgMsg);
			    }

			    unit.lastFrameLoopedAction = true;
		    }
		    else
		    {
			    unit.lastFrameLoopedAction = false;
		    }
	    }

#if DEBUG_MODE
			if (PlayerPreferences.instance.enableLoopedActionsFinder)
			{
				actionsFinder.Process(Time.realtimeSinceStartup, Time.frameCount);
			}
#endif
    }
}