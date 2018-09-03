XP Runners
==========
[![Build Status on TravisCI](https://secure.travis-ci.org/xp-runners/reference.svg)](http://travis-ci.org/xp-runners/reference)
[![BSD License](https://raw.githubusercontent.com/xp-framework/web/master/static/licence-bsd.png)](https://github.com/xp-runners/reference/blob/master/LICENSE.md)


This is the reference implementation of [XP RFC #0303: Subcommands](https://github.com/xp-framework/rfc/issues/303) supporting Windows, Linux and Mac OS X.

Getting started
---------------
To install the XP runners, you can choose between the generic installer:

```sh
$ curl -sSL https://dl.bintray.com/xp-runners/generic/setup-8.1.4.sh | sh
# ...
```

...and a Ubuntu/Debian package:

```sh
$ echo 'deb https://dl.bintray.com/xp-runners/debian jessie main' | sudo tee -a /etc/apt/sources.list.d/xp.list
$ sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys D401AB61
$ sudo apt-get update
$ sudo apt-get install xp-runners
# ...
```

Then, install the framework and you're all set to go:

```sh
$ composer global require xp-framework/core
# ...

$ xp version
XP 9.6.0 { PHP 7.2.9 & ZE 3.2.0 } @ Windows NT SLATE 10.0 build 17134 (Windows 10) AMD64
Copyright (c) 2001-2018 the XP group
FileSystemCL<$APPDATA/Composer/vendor/xp-framework/core/src/main/php>
FileSystemCL<$APPDATA/Composer/vendor/xp-framework/core/src/test/php>
FileSystemCL<$APPDATA/Composer/vendor/xp-framework/core/src/main/resources>
FileSystemCL<$APPDATA/Composer/vendor/xp-framework/core/src/test/resources>
FileSystemCL<.>
```

Commands
--------
The following commands are built in:

* **version** - Displays version and exits - *also `-v`*
* **eval {code}** - Evaluates code - *also `-e {code}`*
* **write {code}** - Evaluates code, writes result to Console - *also `-w {code}`*
* **dump {code}** - Evaluates code, var_dump()s result - *also `-d {code}`*
* **help [{command}]** - Displays help - *also `-?`*
* **run {class}** - Runs a class, class file or xar
* **ar {operation} [{sources}]** - Works with XAR archives
* **list** - Lists available subcommands - built-in, globally installed and locally available

If no command line arguments are given, the help command is run. If command line arguments are given, but no command is passed, the command defaults to *run*.

Plugin architecture
-------------------
Libraries may provide commands by adding a vendor binary to their composer.json named xp.{vendor}.{name}[.{command}] - for the [test](https://github.com/xp-framework/unittest/blob/master/bin/xp.xp-framework.unittest.test) command, the entry is:

```json
{
    "bin": ["bin/xp.xp-framework.unittest.test"]
}
```

The `xp.{module}.{ucfirst(command)}Runner` class serves as the entry point (if the command name is omitted, it will simply be `xp.{module}.Runner`).

By installing the package globally, it becomes available in any directory.

```sh
$ composer global require xp-framework/unittest
# ...

$ xp test -e '$this->assertTrue(true)'
[.]

♥: 1/1 run (0 skipped), 1 succeeded, 0 failed
Memory used: 1265.94 kB (1416.26 kB peak)
Time taken: 0.000 seconds
```

*XP Runners know about Composer's filesystem locations and its dependency management. Thus, the above is the short form of `./xp.exe -m /path/to/unittest [-m /path/to/dependencies/of/unittest ...] xp.unittest.Runner ...`*

Execution
---------
There are four execution models:

1. Run once, exit (*this is the default*)
2. Watch a directory, spawn process every time a change occurs (*via `-watch {directory}`*)
3. Start the process, keep it running until exits with exitcode 0 or the user presses enter (*via `-supervise`*)
4. Repeated runs, cron-like (*via `-repeat {schedule}`*)

The second model is useful e.g. to implement continuous unittest running, see [here](https://github.com/xp-framework/xp-runners/pull/24).

More features
-------------
Less configuration:

* The timezone is automatically mapped from the OS and need no longer be set via `date.timezone`
* If no `use` setting is defined, a globally or locally composer-installed framework will be used
