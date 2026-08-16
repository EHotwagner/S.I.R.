# Tactical environment protected-subject mutations

The focused Release Match gate was run four times against independently
activated .NET-only mutation seams. Every mutation exited non-zero for its
intended semantic assertion; the restored subject then passed the same command.

| Subject | Mutation | Intended red diagnostic |
|---|---|---|
| edge state | force movement/sight/projectile permeability open | `Closed door did not produce ... blocked sight line` |
| content identity | accept mismatched expected identity | `Stale tactical content identity was not rejected` |
| dependency locality | discard the changed dependency intersection | `Door transition did not selectively invalidate ...` |
| destruction bound | propagate a one-target action to neighbours | `Door transition did not emit bounded work ...` |
| static initial entry | tighten the application-entry ceiling below the measured artifact | `app raw is ... bytes; budget is ...` |
| browser initial route | add an oversized response amount to the measured initial request graph | `Expected: <= 1250000` with a larger received count |

Command shape:

```text
SIR_TACTICAL_MUTATE_<SUBJECT>=1 dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build
```

Restored subject result: pass. The mutation switches compile to constant false
under `FABLE_COMPILER`; they are not browser/runtime controls.

Delivery mutation command shapes:

```text
SIR_DELIVERY_BUDGET_MAX_APP_RAW=<below-measured-bytes> node scripts/test-production-delivery-budget.mjs
SIR_DELIVERY_BROWSER_MUTATE_SUBJECT=initial-bytes npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js tests/SIR.Browser.Tests/production-delivery.spec.js
```
