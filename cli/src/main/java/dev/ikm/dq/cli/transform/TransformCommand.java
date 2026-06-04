package dev.ikm.dq.cli.transform;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import picocli.CommandLine;

import java.nio.file.Path;

@Component
@CommandLine.Command(name = "transform", description = "Transform data into PIQI model")
public class TransformCommand implements Runnable {

	private static Logger LOG = LoggerFactory.getLogger(TransformCommand.class);

	@CommandLine.Option(
			names = {"-p", "--data-provider-id"},
			description = "Identifier for the data provider (e.g., source system or team)",
			defaultValue = "")
	private Path source;

	@CommandLine.Option(
			names = {"-p", "--data-provider-id"},
			description = "Identifier for the data provider (e.g., source system or team)",
			defaultValue = "")
	private Path target;

	@CommandLine.Option(
			names = {"-p", "--data-provider-id"},
			description = "Identifier for the data provider (e.g., source system or team)",
			defaultValue = "")
	private Path script;


	@Override
	public void run() {

	}
}
