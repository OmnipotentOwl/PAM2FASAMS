# PAM2FASAMS
<a name="header1"></a>
[![licence badge]][licence]
[![stars badge]][stars]
[![forks badge]][forks]
[![issues badge]][issues]

Navigation
 - [How to use](#how-to-use)
 - [Reference Documents](#reference-documents)
 - [License MIT](#license)

File conversion utility for Florida PAM 155-2 to FASAMS System. Built on .Net 4.6.1 and targeted at Windows 10 this utility is designed for single user use and does not handle multiple simultaneous operations.



## How to use
Normal operation mode:
```
PAM2FASAMS.exe run [OPTIONS]
-b, --batch        (Default: false) Run utility in batch mode for a directory
-d, --directory    Directory for batch mode
-t, --type         PAM file type
-i, --input        Input file to be processed.
-o, --output       Output file to be generated.
--verbose          (Default: false) Prints all messages to standard output.
--help             Display help screen.
--version          Display version information.
```
When running in batch mode only the directory needs to be specified as the application will loop through all files in the directory.

Admin operation mode:
```
PAM2FASAMS.exe admin [OPTIONS]
-d, --directory    Required. Directory for operation
-t, --type         Required. Administrative task type. (Type Options: DUMP_DB, LOAD_DB, LOAD_FILE)
--verbose          (Default: false) Prints all messages to standard output.
--help             Display help screen.
--version          Display version information.
```

## Reference Documents
[Documentation for PAM 155-2 v12](http://www.myflfamilies.com/service-programs/substance-abuse/pamphlet-155-2-v12)

[Documentation for FASAMS](http://www.myflfamilies.com/service-programs/substance-abuse/fasams)
## License

[The PAM2FASAMS utility uses the MIT License.](LICENSE.md)

[*Back to top*](#header1)

[licence badge]:https://img.shields.io/badge/license-MIT-blue.svg
[stars badge]:https://img.shields.io/github/stars/OmnipotentOwl/PAM2FASAMS.svg
[forks badge]:https://img.shields.io/github/forks/OmnipotentOwl/PAM2FASAMS.svg
[issues badge]:https://img.shields.io/github/issues/OmnipotentOwl/PAM2FASAMS.svg

[licence]:https://github.com/OmnipotentOwl/PAM2FASAMS/blob/master/LICENSE.md
[stars]:https://github.com/OmnipotentOwl/PAM2FASAMS/stargazers
[forks]:https://github.com/OmnipotentOwl/PAM2FASAMS/network
[issues]:https://github.com/OmnipotentOwl/PAM2FASAMS/issues
