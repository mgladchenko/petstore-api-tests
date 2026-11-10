@pet @create
Feature: Create pet
  POST /pet adds a new pet to the store.
  A created pet must be returned in the response and persisted so that it can be read back.

  Scenario: Pet with all fields is created and persisted
    Given a new pet with all fields populated
    When I create the pet
    Then the response status code is 200
    And the response is in JSON format
    And the response contains the pet
    And the pet can be retrieved from the store

  Scenario: Store assigns an ID when the pet is created without one
    Given a new pet with only the required fields and no ID
    When I create the pet
    Then the response status code is 200
    And the store assigns an ID to the pet
    And the pet can be retrieved from the store

  Scenario: Malformed JSON is rejected
    When I send a create pet request with content type "application/json" and body:
      """
      {"id": 1, "name": "Rex",
      """
    Then the response status code is 400
    And the error message is "bad input"

  @knownIssue
  Scenario Outline: Pet without a required field is rejected
    Given a new pet with all fields populated
    When I create the pet without the required field "<field>"
    Then the response status code is 405

    Examples:
      | field     |
      | name      |
      | photoUrls |

  @knownIssue
  Scenario Outline: Pet with an invalid field value is rejected
    Given a new pet with all fields populated
    When I create the pet with field "<field>" set to JSON value '<value>'
    Then the response status code is <expectedStatus>

    Examples:
      | field     | value          | expectedStatus |
      | id        | "abc"          | 400            |
      | photoUrls | "not-an-array" | 400            |
      | name      | null           | 405            |
