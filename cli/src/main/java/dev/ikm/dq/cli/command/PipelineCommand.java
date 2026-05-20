package dev.ikm.dq.cli.command;

import org.springframework.stereotype.Component;
import picocli.CommandLine;

@Component
@CommandLine.Command(name = "pipeline", description = "Runs ingest, evaluate, and backup lifecycle")
public class PipelineCommand implements Runnable {

	@Override
		public void run() {
			System.out.println("Running pipeline...");
		}
}
