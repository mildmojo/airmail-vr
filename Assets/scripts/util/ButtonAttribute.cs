using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.All)]
public class ButtonAttribute : PropertyAttribute {
  public readonly string Caption;
  public readonly string MethodName;

  public ButtonAttribute(string caption, string methodName) {
    Caption = caption;
    MethodName = methodName;
  }
}
