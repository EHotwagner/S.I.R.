
import { singleton } from "./fable_modules/fable-library-js.5.13.0/AsyncBuilder.js";
import { responseFromJson, encodeRequest } from "./SIR.Protocol/Http.js";
import { awaitPromise } from "./fable_modules/fable-library-js.5.13.0/Async.js";
import { Exception } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { Result_DefaultWith } from "./fable_modules/fable-library-js.5.13.0/Result.js";
import { concat } from "./fable_modules/fable-library-js.5.13.0/String.js";
import { toString } from "./fable_modules/fable-library-js.5.13.0/Types.js";

export function bootstrap(request) {
    return singleton.Delay(() => {
        const options = {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-SIR-Development-Actor": request.ActorName,
            },
            body: encodeRequest(request),
        };
        return singleton.Bind(awaitPromise(fetch("/api/bootstrap", options)), (_arg) => {
            const response = _arg;
            return singleton.Bind(awaitPromise(response.text()), (_arg_1) => {
                const body = _arg_1;
                return singleton.Combine(!response.ok ? (((() => {
                    throw new Exception(`bootstrap request failed: ${body}`);
                })(), singleton.Zero())) : singleton.Zero(), singleton.Delay(() => singleton.Return(Result_DefaultWith((error) => {
                    throw new Exception(concat("bootstrap response did not decode: ", error));
                }, responseFromJson(toString(body))))));
            });
        });
    });
}

