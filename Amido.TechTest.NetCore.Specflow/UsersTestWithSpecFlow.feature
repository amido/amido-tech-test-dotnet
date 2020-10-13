Feature: UsersTestWithSpecFlow
	In order to work for Amido
	As a CSharp user
	I want to manage the users API path with SpecFlow, RestSharp and NUnit

Scenario: SpecFlow_ Create user
	Given a new user
	When I request to create a user
	Then I should get a user id for the new user

Scenario: SpecFlow_ Update password
	Given an existing user with an updated password
	When I request to update a user
	Then the user's password should be updated