**Note** : If you have a Zuiki MasCon, this program is, somehow, deprecated. Please now use this one, as it has better support for upcoming hardware and support long key press : https://github.com/cracrayol/ZUIKI_to_JRE

# ConToJREts

## About the software

This is an intermediary support software to play JR EAST Train Simulator with ZUIKI's one-handle controller (ZKNS-001) for Switch or Train Simulator Master Controller 2.

Based on ConToJREts by saha209_ura, decompiled using dotPeek.

## How to use

### ZKNS-001 version

1. Extract the files in the package, and be sure to leave the dll in the same hierarchy as the exe (don't delete it).
2. Insert the controller.
3. Run "ConToJREts.exe", select the controller, and press the "Connect" button.
If the message "connected" appears, it is a success. If not, try reinserting the controller or try another controller.
4. Start JR EAST Train Simulator. Select a train and confirm that it runs.
★★★★★★★★★★★★★★★★★
Be sure to insert the controller into the EB stage first.
★★★★★★★★★★★★★★★★★
5. Let it move.
6. To quit, click "Exit JR EAST Train Simulator" -> "Disconnect" -> "Close the program" button.

No device driver settings are required.


### TS Masscon 2 version

1. Unzip the contents inside. 2.
2. Insert the controller. Set the handles to P5B8.
3. Run "ConToJREts.exe", select the controller, and press the "Connect" button.
If the message "Connected" appears, it is a success. Otherwise, you may have to reinsert the controller or check the COM again.
4. Start JR EAST Train Simulator. Select a train and confirm that it runs.
★★★★★★★★★★★★★★★★★
Be sure to insert the controller into the EB stage first.
★★★★★★★★★★★★★★★★★
5. Let it move.
6. To quit, click "Exit JR EAST Train Simulator" -> "Disconnect" -> "Close the program" button.


## Function

The structure of the software is to convert controller input into mouse/keyboard behavior.
Therefore, various behaviors are unstable.

When the lever is moved to the EB/P5 step, it initializes (or something like that).
Please bring the lever to the 5th step once when the number of steps is shifted.
The lever may also be shifted, so please move the lever or keyboard intervention as necessary.

The configuration of the buttons is made using key.ini file.
The syntax is the one used by SendKeys class. See this : https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys
The whistle and other buttons do not support long presses. I couldn't think of a clever way to implement this, so please don't do it.
Is there any way to press and hold the button without delay?

I think it would be better to use JoyToKey or other software for buttons.
If you check the "disable buttons" checkbox, the software will not output anything other than arrows.
The default setting (i.e., unchecked) still outputs arrows.

There is a notice from the official announcement that the controller will be supported. This is just a temporary feature until then, so we haven't built it in yet.

Before starting the game,
From steam→settings→controllers→general controller settings, be sure to uncheck the "switch settings support" checkbox.
The controller is forcibly converted to a keyboard.

Currently, we do not support any controllers other than ZUIKI controllers and TS MassCon 2.
Please don't report to me if you plug in another controller and it doesn't work.


## Request for cooperation

Thanks to a change in the library, we are now able to identify multiple controllers even if only one is plugged in.
Therefore, there is a possibility that it will not work, especially with ZUIKI controllers.
If it does not work, please check if the following values are the same.

Right click on Start → Device Manager → Right click on HID compliant game controller → Properties
→ Details → Second row of "Hardware ID" in the Properties column

"HID\VID_0F0D&PID_00C1"
"HID\VID_33DD&PID_0001"
"HID\VID_33DD&PID_0002"
"HID\VID_33DD&PID_0004"

If you have a ZUIKI controller with different numbers, please report it.


## Dependencies

We utilized HidLibrary as our library.
https://github.com/mikeobrien/HidLibrary

Thank you very much.

## Update History

2022/09/20	Published
　18:00　		Update: change button setting
　18:50			Update: change the button setting again.
2022/09/21	Update
2022/11/15	Major update
　21:50			Update
2022/11/16	Update: added disable buttons function
2023/02/21	Update
2023/10/12	Update: English traslation, customizable key binding
2024/03/19	Add new Zuiki hardware ID (PID 4)

Created by Saha209(@saha209_ura)wagashi.209@gmail.com
Updated by cracrayol (Fediverse: @cracrayol@friendica.cracrayol.org / BS: @cracrayol.org)
