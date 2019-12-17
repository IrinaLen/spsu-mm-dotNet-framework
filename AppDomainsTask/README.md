# App Domains course task

## Architecture
*CalculatorInterface* contains interface for calculators. 3 calculators are implemented in *CalculatorLibrary*. *AppDomainsTask* contains program which loads `CalculatorLibrary.dll` and picks from it all classes that implement `ICalculator` interface (all this is done in separate domain).

## Tests
This is small *VS Studio solution* and testing is done inside of main program of *AppDomainsTask* project. Just run it and see tests' output in console.

## Run
1. Build and run *AppDomainsTask* solution, dependencies should be resolved automatically

or

1. Build *CalculatorInterface*
2. Build *CalculatorLibrary*
3. Build *AppDomainsTask*
4. Run
