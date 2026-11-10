@pet @delete
Feature: Delete pet
  DELETE /pet/{petId} removes a pet from the store.

  Scenario: Existing pet is deleted
    Given an existing pet in the store
    When I delete the pet
    Then the response status code is 200
    And the response confirms deletion of the pet
    And the pet is no longer in the store

  Scenario: Deleting a pet does not affect other pets
    Given an existing pet in the store
    And another existing pet in the store
    When I delete the pet
    Then the response status code is 200
    And the pet is no longer in the store
    And the other pet is still in the store

  Scenario: Deleting an already deleted pet returns not found
    Given a pet that has been deleted from the store
    When I delete the pet
    Then the response status code is 404

  @knownIssue
  Scenario: Non-numeric pet ID is rejected
    When I delete a pet with ID "abc"
    Then the response status code is 400
    And the response is in JSON format

