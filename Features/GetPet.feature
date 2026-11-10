@pet @get
Feature: Get pet by ID
  GET /pet/{petId} returns a single pet or a "Pet not found" error.

  Scenario: Existing pet is returned with all its data
    Given an existing pet in the store
    When I request the pet by its ID
    Then the response status code is 200
    And the response is in JSON format
    And the response contains the pet

  Scenario: Deleted pet is not found
    Given a pet that has been deleted from the store
    When I request the pet by its ID
    Then the response status code is 404
    And the error message is "Pet not found"

  Scenario: Unused pet ID returns not found
    Given a pet ID not present in the store
    When I request the pet by its ID
    Then the response status code is 404
    And the error message is "Pet not found"

  @knownIssue
  Scenario: Non-numeric pet ID is rejected
    When I request a pet with ID "abc"
    Then the response status code is 400
    And the response is in JSON format
