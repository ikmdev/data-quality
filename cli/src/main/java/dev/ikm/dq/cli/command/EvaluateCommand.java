package dev.ikm.dq.cli.command;

import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.io.File;

@Component
@CommandLine.Command(name = "evaluate", description = "Evaluate data quality rules")
public class EvaluateCommand implements Runnable {

	@CommandLine.Option(names = {"-j", "--run-name"}, description = "Name of the run")
	private String runName;

	@CommandLine.Option(names = {"-r", "--rubric-file"}, description = "File containing the data quality SAMs")
	private File rubricFile;

	@CommandLine.Option(names = {"-p", "--piqi-server-url"}, description = "URL of the Piqi server")
	private String piqiServerURL;

	@Override
		public void run() {
			System.out.println("Evaluating data quality rules...");
		}
}
