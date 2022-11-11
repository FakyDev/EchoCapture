# EchoCapture

EchoCapture is a command-line application allowing you to capture your screen at an
interval time, with configurations of your choice, supported on Windows 10 and Windows 11.

# Requirements

.NET Desktop Runtime 5.x.x, is required to run the application.
[Download Here](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)

# Basic Configuration

### Update save folder location
The default save location of capture screen is found at `%localappdata%\EchoCapture`.
Below is an example on how to change the save folder to `C:\Games\Screenshots`.

```
.setting folder C:\Games\Screenshots
```

### Update capture screen interval

Set the screen capture interval, which is in milliseconds. `(1000ms = 1s)`

Updating the capture interval to 40s.
```
.setting interval 40000
```
