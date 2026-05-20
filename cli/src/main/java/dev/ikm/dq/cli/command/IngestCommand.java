package dev.ikm.dq.cli.command;

import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.io.File;

@Component
@CommandLine.Command(name = "ingest", description = "Ingest data into the system")
public class IngestCommand implements Runnable {

	@CommandLine.Option(names = {"-j", "--job-name"}, description = "Name of the job")
	private String jobName;

	@CommandLine.Option(names = {"-d", "--data-set-file"}, description = "File containing the data set")
	private File dataSetFile;

	@CommandLine.Option(names = {"-s", "--script-file"}, description = "File containing the ingest script")
	private File scriptFile;

	@Override
	public void run() {
		System.out.println("Ingesting data...");
	}

}
