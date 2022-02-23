Feature: FundamentalData
	Check if the fundamental data is being imported and stored 
	as expected

@EmptyInsert
Scenario: Importing random fundamental data   
	Given I have prepared random fundamental data for importing
	When I have connected to the Database Importer Service 
	And I have called for this data to be imported	
	Then the result should be 66564 rows in the database

@NewDataInsert
Scenario: Importing new non-existant fundamental data into already populated database
	Given I had prepared new non-existant fundamental data for importing into already populated database
	When I have connected to the Database Importer Service 
	And I have called for this data to be imported	
	Then the result should be 66564 rows in the database 
	And a total of 5 ADP Report updates for the year 2016

@mytag
Scenario: Importing the fundamental data into already populated database with the same data
	Given I have prepared the fundamental data for double importation
	When I have connected to the Database Importer Service 
	And I have called for this data to be imported	
	Then the result should be 66564 rows in the database

@mytag
Scenario: Importing latest fundamental data to update database data
	Given I had prepared the latest fundamental data for importing into already populated database
	When I have connected to the Database Importer Service 
	And I have called for this data to be imported	
	Then the result should be 66564 rows in the database 
	And a total of 3 ADP Report data 

#@mytag
#Scenario: Importing latest fundamental large dataset to update database data
#
#@mytag
#Scenario: Importing latest fundamental data following consecutive data imports 
#
#
#@mytag
#Scenario: Importing invalid non-existant fundamental data into already populated database
#
#@mytag
#Scenario: Importing errorneous fundamental data into already populated database