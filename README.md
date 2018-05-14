# DynamixelSDKSharp
Object oriented .NET approach to controlling Dynamixel hardware.

```
var port = new Port("COM6", BaudRate.BaudRate_115200);
port.Refresh();

//list servos
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
