package dev.ikm.dq.cli.command;

import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.io.File;

@Component
@CommandLine.Command(name = "ingest", description = "Ingest data into the system")
public class IngestCommand implements Runnable {

	@CommandLine.Option(names = {"-j", "--run-name"}, description = "Name of the run", required = true)
	private String runName;

	@CommandLine.Option(names = {"-d", "--data-file"}, description = "File containing the data", required = true)
	private File dataFile;

	@CommandLine.Option(names = {"-s", "--script-file"}, description = "File containing the ingest script", required = true)
	private File scriptFile;

	@Override
	public void run() {
		System.out.println("Ingesting data...");
	}

}
