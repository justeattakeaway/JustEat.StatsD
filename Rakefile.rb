require 'bundler'
Bundler.require :default
require 'rake/clean'
require 'pathname'
require './automation/logging.rb'
require './automation/teamcity.rb'
require './automation/version.rb'
require './automation/clean.rb'
require './automation/assembly_info.rb'
require './automation/nuget.rb'
require './automation/nunit.rb'
require './automation/ndepend.rb'
include Cocaine

name = ENV['component'] || 'JustEat.StatsD'
@log = setup_logging(ENV['verbosity'] || 'info')
configuration = ENV['msbuild_configuration'] || 'Release'
cmd_opts = {logger: @log}

setup_nunit configuration: configuration, depend_on: [:compile]
setup_ndepend configuration: configuration, in_dirs: FileList.new("out/**/#{configuration}")
setup_nuget name: name, configuration: configuration, version: version

AssemblyInfoGenerator.new(log: @log, version: version).generate

desc 'Bootstrap all build-dependencies'
task :bootstrap => ['nuget:restore', :assembly_info]

desc "Compile solution"
msbuild :compile => [:assembly_info, 'nuget:restore'] do |m|
	m.properties :configuration => configuration
	m.targets :Build
	m.solution = "#{name}.sln"
end
CLEAN.include 'out', '**/obj'
CLOBBER.include 'packages'


task :build => [:bootstrap, :assembly_info, :compile, :test, :analyse]
task :package => [:build, :nuget]
task :default => [:package]
