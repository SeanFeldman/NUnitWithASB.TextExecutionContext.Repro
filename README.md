# NUnit TestExecutionContext is null when running ASB callback

NUnit 3.7.0 has introduced a controlled change on how `TestExecutionContext` is returned
- In version 3.6.1 it was [guarding again null](https://github.com/nunit/nunit/blob/3.6.1/src/NUnitFramework/framework/Internal/TestExecutionContext.cs#L219)
- In vesion 3.7.0 [guarding was removed](https://github.com/nunit/nunit/blob/3.7/src/NUnitFramework/framework/Internal/TestExecutionContext.cs#L192) an [assumption was made](https://github.com/nunit/nunit/issues/2223#issuecomment-306458085) that assertions should be executed under test thread. In case assertion is not running under a test thread, [assertions will fail](https://github.com/nunit/nunit/issues/2223).

With WindowsAzure.ServiceBus ASB client, message pumps are executed as separate threads, so any assertion would be considered to be outside of the testing thread.
This repro demonstrates that.

Questions:

1. Should NUnit assumption be challenged?
1. What does ASB Client do to cause `CallContext.GetData(CONTEXT_KEY)` to be `null`?
