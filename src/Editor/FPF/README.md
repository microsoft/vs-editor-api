# FPF: Fake Presentation Foundation

FPF stubs just enough of the WPF APIs that the editor depends on. Notably
we require some primitives such as `Color`, `Brush`, `Point`, etc. This is
a very minimal approach to bootstrap the editor on macOS, and over time we
anticipate eliminating this support layer entirely.

For `WindowsBase`, we have simply imported a subset of
[Mono's version][mwb] and made changes to the `Dispatcher` to use macOS'
[Grand Central Dispatch][gcd], since the editor uses this extensively,
we wanted it to be highly performant.

For the other assemblies we generated necessary stubs and filled in the
basic primitives.

FPF depends on Xamarin.Mac to build at this point.

[mwb]: https://github.com/mono/mono/tree/master/mcs/class/WindowsBase
[gcd]: https://developer.apple.com/documentation/dispatch