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

```
Copyright (c) 2018 Kimchi and Chips

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
