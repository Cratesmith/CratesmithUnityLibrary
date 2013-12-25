using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

// Extensions to the RuntimeAnimationController class
// the rest of this file is just a build/play time constructed database in each scene to support these
public static class RuntimeAnimatorControllerExtension
{
	public static ControllerState					GetCurrentState(this Animator ac, int layer) 	{ var hash = ac.GetCurrentAnimatorStateInfo(0).nameHash; return ac.GetStates().FirstOrDefault((arg) => hash == arg.uniqueNameHash); }
	public static IEnumerable<ControllerParameter> 	GetParameters(this Animator ac) 	{ return RuntimeAnimatorControllerExtensionData.GetParameters(ac.runtimeAnimatorController); }
	public static IEnumerable<ControllerState>		GetStates(this Animator ac) 		{ return RuntimeAnimatorControllerExtensionData.GetStates(ac.runtimeAnimatorController); }
	public static IEnumerable<ControllerLayer>		GetLayers(this Animator ac) 		{ return RuntimeAnimatorControllerExtensionData.GetLayers(ac.runtimeAnimatorController); }
	public static int 								GetLayerCount(this Animator ac) 	{ return RuntimeAnimatorControllerExtensionData.GetLayerCount(ac.runtimeAnimatorController); }
}
