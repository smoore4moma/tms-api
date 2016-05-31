#tms-api

This version of the TMS API represents a substantial change from previous iterations.  There are now the following elements:

- OAuth for authentication
- A Single Page Application web interface
- Help pages that show url structure, parameters, and json/xml data samples
- Language identifier
- Institution identifier
- Improved API conventions including result counts and objects for foreign key data
- Data visualizations that use the API
 
There is an important distinction about the TMS API - the data are platform and language agnostic.  It's JSON and XML via REST services.  If anything, it is oriented toward JavaScript and HTML5.  The source code, though, is certainly Microsoft-dependent.  The goal of making the code open source is to allow you to build something similar if MS technologies are what you use.  

##tms-web

 >The tms-web project is a HTML5 web front-end to tms-api:  https://github.com/smoore4moma/tms-web

##screenshots

<strong>UI</strong>

<img src='https://github.com/smoore4moma/tms-api/blob/master/tms-api/Images/tms-api.jpg' />

<strong>Help</strong>

<img src='https://github.com/smoore4moma/tms-api/blob/master/tms-api/Images/tms-api-help.jpg' />

<strong>Visualization</strong>

<img src='https://github.com/smoore4moma/tms-api/blob/master/tms-api/Images/tms-api-example.jpg' />


<strong>Installation</strong>

Much of this project comes straight out of Visual Studio 2013 Single Page Application template.  In fact, this is really where you should start.  The parts that are museum-specific are Models, Views and Controllers with the words Object/Artist/Exhibition/Term.  You can add these to an existing project after forking the Git repository.

This project requires you to know SQL Server, and TMS for that matter, fairly well.  Please see the database directory and the README.md file there for database setup and configuration.  The project is validated againt TMS 2012 SP1 and SP2 running on SQL Server 2008. 
