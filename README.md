# DynamixelSDKSharp
![Dynamixel motor image](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/.resources/dynamixel_x_04.png?raw=true)

## Introduction

A .NET library for controlling Dynamixel actuator products from [Robotis](http://www.robotis.com/).

This library's aims:

* Easy to use
* Managed and Object Oriented
* Utilities for common use patterns with Dynamixel Servos
* User's code should be the same regardless of which servo products are used

The library code can be cross-platform, but it has only been tested on Windows.

## Sample code

```C#
// Open a COM port
var port = new Port("COM6", BaudRate.BaudRate_115200);

// Search for all servos on this port
port.Refresh();

// Iterate through servos
foreach (var servoKeyValue in port.Servos)
{
  var servoID = servoKeyValue.Key;
  var servo = servoKeyValue.Value;

  // Enable torque on this servo
  servo.WriteValue(RegisterType.TorqueEnable, 1);

  // Move through range of positions
  for (int i = 0; i < 4096; i += 5)
  {
    servo.WriteValue(RegisterType.GoalPosition, i);
  }
}
```

## Features

* Automatically detect Ports
* Automatically detect Servos
* Ports are threaded safely
* Register address tables can be defined in JSON (we don't need to hard-code address tables for each servo)
* DynamixelREST sample application provides REST API (e.g. can control Servos from remote devices)

## Classes

* Port
* Servo
* Register

## DynamixelREST

This is a standalone application which provides a REST API for manipulating Dynamixel actuator products. With this, it becomes easy to create applications in any language (e.g. JavaScript, Python, Processing, etc) which interact with the Dynamixel hardware.

To see a full list of REST commands see here:
https://github.com/elliotwoods/DynamixelSDKSharp/tree/master/Samples/Dispatcher/Dispatcher/Requests

Note that the addresses for each request are shown in the class decorators, or else denoted by the class name and namespace.

Dispatcher also supports logging to a local MongoDB server if available. If you would like to disable this feature, then please remove this section from the Schedule.json file:

```json
		{
			"Period": 0,
			"Action": "initDataLogger",
			"OnStart": "true"
		},
```

The scheduler defines regular tasks which you would like to perform:

* `Period` defines an interval in seconds between each action. `0` signifies that the schedule is disabled (e.g. used with `OnStart`, this can mean that it only happens on start of application).
* `Action` defines a REST endpoint which will be performed at each schedule interval
* `OnStart` defines whether this endpoint will be performed at application start.

## License

DynamixelSDKSharp is by Kimchi and Chips and is available under the [MIT License](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/LICENSE)
NativeFunctions.cs and the native DLL's are from the DynamixelSDK by Robotis, made available under the [Apache License 2.0](https://github.com/ROBOTIS-GIT/DynamixelSDK/blob/master/LICENSE)

Dynamixel is a trademark of Robotis Ltd. (Korea).
