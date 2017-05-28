using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonAttributeDrawer : DecoratorDrawer {
  ButtonAttribute buttonAttr { get { return (ButtonAttribute) attribute; } }

  public override float GetHeight() {
    return base.GetHeight() + 10f;
  }

  // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
  public override void OnGUI(Rect position) {
Debug.Log("Drawing!");

    var rect = new Rect(position.x, position.y, position.width, 20f);
    var caption = buttonAttr.Caption;
    var methodName = buttonAttr.MethodName;

    if (GUI.Button(rect, caption)) {
      List<Component> components = new List<Component>();
      Selection.activeGameObject.GetComponents(components);
      Debug.Log(components.Count);
      // Component component = components.Find(comp => {
      //   var fields = new List<FieldInfo>(comp.GetType().GetFields());
      //   return fields.Find(field => {
      //     var attrs = field.GetCustomAttributes();
      //     return attrs.Find(attr => attr == buttonAttr);
      //   });
      // });
      // Debug.Log(component && component.name);

      // MethodInfo tMethod = theObject.GetType().GetMethod(methodName);
      // if (tMethod != null) tMethod.Invoke(theObject, null);
      Debug.Log("Oh my button.");
    }
  }

}
