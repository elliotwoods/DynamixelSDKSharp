# Introduction

This application is used for a necessary self-calibration of the modules. This should be run on every module individually after the yoke has been re-assembled. It handles:

* Testing that both axes are working
* Aligning A2 axis to vertical before you tighten the yoke-axis screw
* Finding the limits of both axes and storing as soft limits in the motor

The procedure is guided by the application. Generally it involves:

1. Attach a single module to the computer. The yoke-A2 screw should be loose
2. Check serial port settings (there is a json file in the app folder. Keey 115200 but change the port name to match the real port name)
3. Run the app
4. Choose `1 - Calibrate` and follow the steps

# Last known working version

The HaloTestHarness is tested with the version of DynamixelSDKSharp from 2021-06-01. This version is a 'multithreaded' version. We tried (2023-11-16) with the recent singlethreaded branch and the code didn't work.

Test setup that worked (2023-11-16 at Macau) :

* Grey plastic USB to RS485
  * Wires
    * Green -> T+
    * Red -> T-
  * Switches
    * Ne
    * RS485
* 12V PSU in-line injector
* Servo Motors 89, 90
* No terminator resistor (actually with terminator also works?)
* COM4
* Dynamixel Wizard is installed (maybe this changes things?)