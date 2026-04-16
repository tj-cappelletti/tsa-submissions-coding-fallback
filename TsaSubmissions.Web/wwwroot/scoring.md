# Scoring of Submissionms
The scoring of a participant's submission is achieved via a weighted composite scoring.
We use three aspects or components to evaluate the submission.
These values are used to calculate a final score based on their weighted values.

```
Score = (Correctness × 0.60) + (Efficiency × 0.20) + (Quality × 0.20)
```

| Component          | Weight | Description                                                                     |
|--------------------|--------|---------------------------------------------------------------------------------|
| Correctness        | 60%    | Number of test cases that have passed                                           |
| Efficiency Ratio   | 20%    | The performance of the solution relative to a known good baseline               |
| Code Quality Ratio | 20%    | The quality of the solution using standard metrics relative to a known baseline |

Both the Efficiency Ratio and Code Quality Ratio are ratios calculated by comparing the participant's value to a known baseline solution's value.
For each supported language, an optimized solution is created and measured just as a participant's entry would be.
The test cases for a given problem are executed against the optimized solution 100 times.
The overall run duration and individual test case run durations results are captured and averaged out.
This is to ensure caching, CPU, and/or memory issues do not effect the baseline value.
The metrics used in code quality are also captured and set at the known baseline for the problem and langauge combination.

## Correctness
The correctness score is intentionally weighted as the heaviest as the solution must work (even partially) to be considered for the competition.
Regardless of chosen programming language, all submissions are evaluated against the same test cases.
The score is based on the number of test cases that have passed.

```
Correctness = (Total Passed Test Cases / Total Test Cases) x 100
```

For example:

| Submission             | Total Passed Test Cases | Total Test Cases | Correrctness     |
|------------------------|-------------------------|------------------|------------------|
| Team 9001-901 (Java)   | 30                      | 30               | 100              |
| Team 9001-901 (Python) | 28                      | 30               | 93.333           |

## Efficiency Ratio
The Efficiency Ratio is designed to reward problem solving skills and penalize brute force approaches to the problem.
Each programming language has its own performance, strengths, and weaknesses that must be considered when building a solution.
In order to make the Efficiency Ratio score language agnostic as much as possible, and to ensure fairness when comparing across submissions, we use a known baseline for the chosen language to compare the efficiency of the solution against to create the ratio.

```
Efficiency Score = (Baseline Time / Participant Time) × 100
```

For example:

| Submission             | Baseline Time | Participant Time | Efficiency Score |
|------------------------|---------------|------------------|------------------|
| Team 9001-901 (Java)   | 120ms         | 190ms            | 63.157           |
| Team 9001-901 (Python) | 200ms         | 220ms            | 90.909           |

In this situation, the Python code is more efficient (as compared to the baseline).
It's important to note that if a participant's solution is faster then the baseline, the score will reflect that reward the participant for their efforts.

## Code Quality Ratio
The Code Quality Ratio is designed to reward solutions that are written in such a way that future engineers would have no issues understaning the code and be able to easily maintain it.
When it comes to code quality, there is a level subjectivity at play which is why the weighted value for this score is low.
Just as with the Efficiency Score, we use a baseline value from the same solution used to calculate the baseline Efficiency Score.
To calculate the Code Quality Ratio score, we use two industry established metrics to gauage quality; Cyclomatic Complexity and Lines of Code.

```
Cyclomatic Complexity Score = (Baseline Cyclomatic Complexity / Participant Cyclomatic Complexity) x 100

Lines Of Code Score = (Baseline Lines of Code / Participant Lines of Code) x 100

Code Quality Ratio Score = (Cyclomatic Complexity Score x 0.75) + (Lines Of Code Score x 0.25)
```

Cyclomatic Complexity is a software metric used to measure the structural complexity of a program by counting the number of linearly independent paths through its source code.
The lower the number, the lower the percieved complexity.
While this is not a perfect way to measure complexity, using a baseline value to help gauge if participant's solution is more or less complex then a known good solution allows us to compare complexity across submissions.

The Lines of Code metric is often the subject of scrutiny and not always an ideal metric.
We use this value in comparison to the known good solution with the purpose of gauging the particpant's command of the language they have chosen.
Each programming language has tips and tricks to achieve certain outcomes easily.
While again, not perfect, this does help to guage the participant's command as many of these tricks result is less code being written.

For example:

| Submission             | Baseline Cyclomatic Complexity | Participant Cyclomatic Complexity | Cyclomatic Complexity Score | Baseline Lines of Code | Participant Lines of Code | Lines Of Code Score | Code Quality Ratio Score |
|------------------------|--------------------------------|-----------------------------------|-----------------------------|------------------------|---------------------------|---------------------|--------------------|
| Team 9001-901 (Java)   | 10 | 12 | 83.333 | 20 | 38 | 52.631 | 75.656 |
| Team 9001-901 (Python) | 15 | 29 | 51.724 | 40 | 46 | 86.956 | 61.282 |

## Final Score
Using the examples listed above, here is how the final scores would calculate out:

| Submission             | Correctness | Efficiency | Quality | Final Score |
|------------------------|-------------|------------|---------|-------------|
| Team 9001-901 (Java)   | 100         | 63.157     | 75.656  | 87.763      |
| Team 9001-901 (Python) | 93.333      | 90.909     | 61.282  | 86.438      |