using UnityEngine;
using UnityEngine.InputSystem;

public static class InitCustomDeviceUsages
{
	static InitCustomDeviceUsages()
	{
		Initialize();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		InputSystem.RegisterLayoutOverride("\n            {\n                \"name\" : \"GamepadPlayerUsageTags\",\n                \"extend\" : \"Gamepad\",\n                \"commonUsages\" : [\n                    \"Player1\", \"Player2\"\n                ]\n            }\n        ");
	}
}
