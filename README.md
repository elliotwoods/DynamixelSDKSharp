# DynamixelSDKSharp
![Dynamixel motor image](https://github.com/elliotwoods/DynamixelSDKSharp/blob/master/dynamixel_x_04.png?raw=true)
Object oriented .NET approach to controlling Dynamixel hardware.

## Sample
```
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
