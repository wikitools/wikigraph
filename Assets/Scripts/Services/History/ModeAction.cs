using Model;
using Services.History;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeAction : UserAction {
	public static Action changeModeAction;

	private static void passChangeMode() {
		changeModeAction();
	}

	public void Execute() {
		passChangeMode();
	}

	public void UnExecute() {
		passChangeMode();
	}
}
