# EchoCapture

EchoCapture is a command-line application allowing you to capture your screen at an
interval time, with configurations of your choice, supported on Windows 10 and Windows 11.

# Requirements

.NET Desktop Runtime 5.x.x, is required to run the application.
[Download Here](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)

# Basic Configuration

### Set save folder location

The default save location of capture screen is found at ``%localappdata%\EchoCapture``.
Below is an example on how to change the save folder to ``C:\Games\Screenshots``.

```
.setting folder C:\Games\Screenshots
```

### Set capture screen interval

The default interval is 10s ``(equivalent to 10000ms)``.
Below is an example on how to change the capture screen interval.

``Note: 1000ms = 1s``

```
.setting interval 40000
```

### Display configuration

Below show the configuration that you are using and the presets configuration.

```
.information
```

# Capturing Screen

### Begin screen capture

Below start capturing your screen at the interval you set.

```
.task start
```

### Stop screen capture

Below end the capture screen task. ``Note: Closing the application will
also end the task.``

```
.task stop
```

# Image Configuration

## General image setting

These settings are applied to each capture capture.

### Set image type

The capture screen can either be in ``png`` or ``jpeg``. [Click Here](https://undsgn.com/jpg-vs-png/#:~:text=The%20Difference%20between%20PNG%20and%20JPG&text=PNG%20stands%20for%20Portable%20Network,%2Dcalled%20%E2%80%9Clossy%E2%80%9D%20compression.)
to find the difference and select what fits you.

```
.imageSetting imageType png
```
