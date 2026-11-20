# Common

## Request-scoped HTTP headers

Pipeline components can call `RequestHeaderScope.MarkRequestScoped(request, headerName)`
when adding a header that must not be inherited by LRO polling or final GET requests.
The policy-token stamper declares `x-ms-policy-external-evaluations` this way after
successfully adding a token. It does not remove headers from GET requests.

The helper publishes immutable header-name snapshots under
`HttpRequestMessage.Properties["Microsoft.Azure.PowerShell.RequestScopedHeaders"]`,
using an `IEnumerable<string>` contract shared with the AutoRest PowerShell
runtime. Matching is case-insensitive; retry clones retain the header and its
scope metadata. Updating the common library alone does not change older generated
modules: they must also be regenerated with the LRO follow-up helper.