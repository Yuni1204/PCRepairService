# PCRepairService

For Postgres Container:
> psql -U postgres -d PCRepairDB (for connecting to DB)
> \dt for display tables, \l to list all schema
> if PCRepairDB=# -> sql commands for checking stuff in tables

container for rabbitmq default from https://www.rabbitmq.com/docs/download
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management

docker-compose:
cd C:\dev\BA_Prototype\PCRepairService
docker-compose up	



