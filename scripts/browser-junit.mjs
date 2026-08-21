const summaryPattern = /^<\?xml version="1\.0" encoding="UTF-8"\?>\n<testsuites tests="(\d+)" failures="(\d+)" skipped="(\d+)">\n <testsuite name="sir-browser" tests="(\d+)" failures="(\d+)" skipped="(\d+)">\n([\s\S]*)\n <\/testsuite>\n<\/testsuites>\n$/u;
const testcasePattern = /  <testcase[\s\S]*?<\/testcase>/gu;

export const parseBrowserShardJUnit = (source, label = "browser shard") => {
  const match = summaryPattern.exec(source);
  if (!match) throw new Error(`${label} wrote malformed deterministic JUnit`);
  const counts = match.slice(1, 7).map(Number);
  if (counts.some((value) => !Number.isSafeInteger(value) || value < 0)) {
    throw new Error(`${label} wrote invalid deterministic JUnit counts`);
  }
  const [tests, failures, skipped, suiteTests, suiteFailures, suiteSkipped] = counts;
  if (tests !== suiteTests || failures !== suiteFailures || skipped !== suiteSkipped) {
    throw new Error(`${label} wrote inconsistent deterministic JUnit summaries`);
  }
  const body = match[7];
  const cases = body.match(testcasePattern) ?? [];
  if (cases.length === 0 || cases.join("\n") !== body) {
    throw new Error(`${label} wrote empty or unreadable deterministic JUnit cases`);
  }
  const actualFailures = cases.filter((value) => value.includes("<failure ")).length;
  const actualSkipped = cases.filter((value) => value.includes("<skipped/>")).length;
  if (tests !== cases.length || failures !== actualFailures || skipped !== actualSkipped) {
    throw new Error(`${label} wrote count-drifted deterministic JUnit`);
  }
  return cases;
};

export const mergeBrowserShardCases = (groups) => {
  const cases = groups.flat().sort((left, right) => left.localeCompare(right));
  if (new Set(cases).size !== cases.length) {
    throw new Error("browser shards wrote duplicate deterministic JUnit cases");
  }
  const failures = cases.filter((value) => value.includes("<failure ")).length;
  const skipped = cases.filter((value) => value.includes("<skipped/>")).length;
  return [
    '<?xml version="1.0" encoding="UTF-8"?>',
    `<testsuites tests="${cases.length}" failures="${failures}" skipped="${skipped}">`,
    ` <testsuite name="sir-browser" tests="${cases.length}" failures="${failures}" skipped="${skipped}">`,
    cases.join("\n"),
    " </testsuite>",
    "</testsuites>",
    "",
  ].join("\n");
};
