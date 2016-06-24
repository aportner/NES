using System;
using System.IO;
using UnityEngine;
using NES;

public class Main : MonoBehaviour
{
	private RenderTexture texture;

	private NES.Console console;

	void Start() {
		var loader = new INES ();
		var stream = new FileStream (Application.dataPath + "/mario.nes", FileMode.Open);
		var reader = new BinaryReader (stream);
		var cart = loader.Load (reader);

		console = new NES.Console ();
		console.Load (cart);

		texture = new RenderTexture (256, 240, 0);
		Camera.main.targetTexture = texture;
	}


	void Update() {
		console.StepSeconds (Time.deltaTime);

		Graphics.Blit (console.ppu.back, texture);
		Graphics.Blit (console.ppu.front, texture);
	}
}

