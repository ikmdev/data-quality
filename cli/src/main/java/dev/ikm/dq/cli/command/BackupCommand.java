package dev.ikm.dq.cli.command;

import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.io.File;

@Component
@CommandLine.Command(name = "backup", description = "Backup the data quality system")
public class BackupCommand implements Runnable {

	@CommandLine.Option(names = {"-j", "--run-name"}, description = "Name of the run")
	private String runName;

	@CommandLine.Option(names = {"-b", "--backup-file"}, description = "File to backup the data quality system")
	private File backupFile;

	@Override
		public void run() {
			System.out.println("Backing up data quality system...");
		}
}
