# Common

## Change Safety request processing

`PolicyTokenAcquirer.StampPolicyTokenAsync` acquires and attaches the
`x-ms-policy-external-evaluations` header to eligible writes when requested.
For GET and HEAD requests, it removes an existing policy-token header without
acquiring another token. This includes headers inherited by LRO polling and
final GETs. Other headers and write/retry behavior are unchanged.

The existing method name and signature are unchanged, so Az.Accounts does not
need recompilation to call it.
Generated modules that already install the Change Safety pipeline step do not
need regeneration for this cleanup. Requests that bypass the step are not
affected, and even explicitly supplied policy-token headers are removed from
GET/HEAD requests that pass through it.