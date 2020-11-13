using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentNotFoundException : System.Exception {

	public ComponentNotFoundException(string errorMessage) : base(errorMessage) { }

}
