This is a test project to showcase my skills as a full stack developer. The project is composed of a Web API project in .net core which uses entity framework and PostgreSQL as the database
and angular for the project front end. When running the .net project and angular front end be sure to check that the .net project is ran in https://localhost:7296/ it should be that by default
but if by any chance the .net project is ran in a different port be sure to change that in base.service.ts in the angular code.

You can clone the repository with Git and then be sure to find the folder where the docker file is and do docker compose up that will set up the image with the database. After that you do need to run
dotnet ef database update to run the migrations.
After that you can do npm i --legacy-peer-deps  and run the angular project and register an account, log in add a pothole and see the list of potholes. Pothole list is public and does not need login but creating is guarded under
an authorize attribute on the controller.
The project is just a toy test but it works and has it's corresponding tests on back and front.
I hope you like it.
Regards,
Cesar
