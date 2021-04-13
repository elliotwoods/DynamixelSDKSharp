# DynamixelSDKSharp
![Dynamixel motor image](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/dynamixel_x_04.png?raw=true)

Simple to use object oriented .NET library for controlling Dynamixel actuator products from [Robotis](http://www.robotis.com/).

```C#
var port = new Port("COM6", BaudRate.BaudRate_115200);
port.Refresh();

foreach (var servoKeyValue in port.Servos)
{
  var servoID = servoKeyValue.Key;
  var servo = servoKeyValue.Value;

  servo.WriteValue(RegisterType.TorqueEnable, 1);

  for (int i = 0; i < 4096; i += 5)
  {
    servo.WriteValue(RegisterType.GoalPosition, i);
  }
}
```

# Features
* Automatically detect Ports
* Automatically detect Servos
* Ports are threaded safely
* Register address tables can be defined in JSON
* Dispatcher sample application provides REST API (e.g. can control Servos from remote devices)

# # Classes

* Port
* Servo
* Register

## Dispatcher

This is a standalone application which runs a server which allows you to manipulate all attached Dynamixel actuators on all ports.

The dispatcher is controlled via REST commands. To see a full list of commands see here:
https://github.com/elliotwoods/DynamixelSDKSharp/tree/master/Samples/Dispatcher/Dispatcher/Requests

Note that the addresses for each request are shown in the class decorators.

Dispatcher also supports logging to a local MongoDB server if available. If you would like to disable this feature, then please remove this section from the Schedule.json file:

```json
		{
			"Period": 0,
			"Action": "initDataLogger",
			"OnStart": "true"
		},
```

# Troubleshooting

## Missing DLL

Error looks like:

```
Couldn't open serial port: Unable to load DLL 'dxl_x86_c.dll': The specified module could not be found. (Exception from
HRESULT: 0x8007007E)
```

This DLL needs to be found by your exe. Generally first try switching the target from "Any CPU" to "x64" in the Visual Studio toolbar.

## NewtonSoft not found

Sometimes you might get build errors that NewtonSoft is not found. Try:

* Right click on your solution
* Select `Restore NuGet Packages`

If that does not work, then try removing the reference from the DynamixelSDKSharp project and adding it again from NuGet.

# License

DynamixelSDKSharp is by Kimchi and Chips and is available under the [MIT License](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/LICENSE)
NativeFunctions.cs and the native DLL's are from the DynamixelSDK by Robotis, made available under the [Apache License 2.0](https://github.com/ROBOTIS-GIT/DynamixelSDK/blob/master/LICENSE)

Dynamixel is a trademark of Robotis Ltd. (Korea).
