XP Runners change log
=====================

## 7.0.0 / ????-??-??

* Added support for ANSI color escape sequences inside Windows Console
  (@thekid)
* Implemented plugin architecture based on Composer and file naming conventions
  `xp.{vendor}.{name}[.{command}]` => `xp.{module}.{ucfirst(command)}Runner`
  (@thekid)
* Added mapping for Windows Timezones to `date.timezone setting`. No more 
  configuration via xp.ini necessary anymore now.
  Inspired by https://github.com/lmcnearney/timezoneinfo-olson-mapper
  (@thekid)
* Initial implementation of xp-framework/rfc#303 with the following commands:
  * **version** - Displays version and exits - *also `-v`*
  * **eval [code]** - Evaluates code - *also `-e [code]`*
  * **write [code]** - Evaluates code, writes result to Console - *also `-w [code]`*
  * **dump [code]** - Evaluates code, var_dump()s result - *also `-d [code]`*
  * **help** - Displays help - *also `-?`*
  * **run** - Runs a class
  (@thekid)