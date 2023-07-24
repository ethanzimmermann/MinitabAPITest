#Minitab Coding Exercise Submission

This is an API that mocks saving customer data, provided as a JSON object, to a CRM.

## Runtime

.Net 7

## Mock CRM functionality

Instead of writing to a CRM, this application writes customer data to a Minitab directory in AppData/Roaming or the defined Application Data folder

## Shortcomings

May have gotten a little more into the weeds with validation than was desired. This was done in order to make unit test cases more easily defined and measured.

Configured the application so that it could handle the camel case in the example provided, however the trailing comma after "US" would not be accepted.

The USPS web service will recognize an address as valid even when it receives an invalid 5 digit zip code (maybe because it is an optional field?). In this case, the USPS web service returns a valid zip code in its response, however the current solution is not parsing the response and instead just saves the data as is. This means that this solution can currently save bad data in that case. Since the exercise description did not mention parsing, I'm leaving that be for now, however I can come back around and change this approach if that would be preferable.
UPDATE: I went back and added the appropriate parsing for completionists sake.