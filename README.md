# DynamixelSDKSharp
![Dynamixel motor image](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/dynamixel_x_04.png?raw=true)

Clean, modern object oriented .NET library for controlling Dynamixel actuator products from [Robotis](http://www.robotis.com/).

## Features
* Automatically detect Ports
* Automatically detect Servos
* Ports are threaded safely
* Register address tables can be defined in JSON
* Dispatcher sample application provides REST API (e.g. can control Servos from remote devices)

## Sample
```C#
var port = new Port("COM6", BaudRate.BaudRate_115200);
port.Refresh();

foreach (var servoKeyValue in port.Servos)
{
  var servoID = servoKeyValue.Key;
  var servo = servoKeyValue.Value;

  servo.WriteValue(RegisterType.TorqueEnable, 1);

  //forwards
  for (int i = 0; i < 4096; i += 5)
  {
    servo.WriteValue(RegisterType.GoalPosition, i);
  }
}
```

## Classes

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

## License

DynamixelSDKSharp is by Kimchi and Chips and is available under the [MIT License](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/LICENSE)
NativeFunctions.cs and the native DLL's are from the DynamixelSDK by Robotis, made available under the [Apache License 2.0](https://github.com/ROBOTIS-GIT/DynamixelSDK/blob/master/LICENSE)
