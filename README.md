# DynamixelSDKSharp
![Dynamixel motor image](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/dynamixel_x_04.png?raw=true)

Clean, modern object oriented .NET library for controlling Dynamixel actuator products from [Robotis](http://www.robotis.com/).

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

The dispatcher is controlled via REST commands.

## License

DynamixelSDKSharp is by Kimchi and Chips and is available under the [MIT License](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/LICENSE)
NativeFunctions.cs and the native DLL's are from the DynamixelSDK by Robotis, made available under the [Apache License 2.0](https://github.com/ROBOTIS-GIT/DynamixelSDK/blob/master/LICENSE)
