using UnityEngine;
using UnityEditor;
using Gruffdev.BCS;

namespace Gruffdev.BCSEditor
{
	public class ActorConfigEditorInstance : ScriptableObject
	{
		public Editor[] editors = new Editor[0];
	}
}
