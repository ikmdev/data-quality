::Postgres run command:
::npm run process:lab -- --excel C:\\data\\benchmark_sample.xlsx --api-url http://localhost:5025/PIQI/ScoreAuditMessage --data-provider-id PROVIDER --data-source-id SOURCE --pg-database piqi --pg-user admin --pg-password admin --rubric Basic_VA_Lab     
::
::Access run command:
::npm run process:lab -- --excel C:\\data\\benchmark_sample.xlsx --api-url http://localhost:5025/PIQI/ScoreAuditMessage  --access-db C:\data\piqi-audit.accdb  --data-provider-id PROVIDER --data-source-id SOURCE

npm run process:lab -- --excel C:\\data\\benchmark_sample.xlsx --api-url http://localhost:5025/PIQI/ScoreAuditMessage --data-provider-id PROVIDER --data-source-id SOURCE --pg-host 10.16.229.172 --pg-database data_quality --pg-user dq --pg-password dq_password --rubric Basic_VA_Lab     


::npm run process:lab -- --excel C:\\data\\benchmark_sample.xlsx --api-url http://localhost:5025/PIQI/ScoreAuditMessage --data-provider-id PROVIDER --data-source-id SOURCE --pg-host localhost --pg-database piqi --pg-user admin --pg-password admin --rubric Basic_VA_Lab     



::npm run process:lab -- --excel C:\\data\\KBS_DATAPULL_1OF3_V3.xlsx --api-url http://localhost:5025/PIQI/ScoreAuditMessage --data-provider-id PROVIDER --data-source-id SOURCE --pg-database piqi --pg-user admin --pg-password admin --rubric Basic_VA_Lab     
