-- Create external DB AD group for SQL developers
CREATE USER [R-XXXXXXXX-Developers] FROM EXTERNAL PROVIDER;

-- Create external DB user for app managed service identity
CREATE USER [AS-XX-DataFileProc] FROM EXTERNAL PROVIDER;
