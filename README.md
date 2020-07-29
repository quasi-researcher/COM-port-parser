# COM-port-parser
### utility to parse and log messages from COM port

Those who deal with hardware often need to read messages from devices connected to your PC over COM ports. This Windows desktop utility helps to read, log and visualize messages from a COM port.
Here below I'll provide the details of how to use the utility. If you have any specific questions you can reach out to me in my blog https://research-based.blogspot.com/
<img src="./gui_1.PNG" width="500" height="300">

## COM port settings
This part of UI allows to choose the appropriate settings. First of all select from the list the corresponding COM port your hardware device connected to. Then select appropriate baudrate, number of bits, options for parity and stop bits. Once ready click "Open". The default settings are 9600,8,N,1.

![Screenshot](gui_port.PNG)

## Data log settings
Once the port is correctly opened the utility can log arriving messages to a .txt file. The default name for the log file is generated automatically as hhmmss_ddmmyyyy_comport_log.txt. You may change the name and directory. Click "Start" to start writing the messages and "Stop" to finish it. You may also add a timestamp to every ariving message by checking the corresponding option. Below is an example of a log file with disabled and enabled timestamp option.
![Screenshot](gui_log.PNG)
![Screenshot](gui_data_txt.PNG)

## Arriving messages
Once the port is correctly opened and the messages are arriving they will be displayed as is in the "Recieved data" window:
![Screenshot](gui_data.PNG)

This is the basic functionality of the utility. You can set the connection, read, log and print the arriving messages. The additional functionality of the utility allows to plot the numbers from the arriving messages.

## Splitting the arriving message
