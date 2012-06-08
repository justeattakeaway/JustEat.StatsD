class NDepend
  def create_parameters
    params = []
    params << File.expand_path(@project_file).gsub('/','\\') #NDepend v3 forces absolute windows-path
    return params
  end
end

def setup_ndepend(params={})
	third_party_path = params[:third_party_path] || '3rdparty'
	project_file = params[:project_file] || FileList.new('*.ndproj').first
	in_dirs = params[:in_dirs] || [File.join('out', 'AnyCPU', 'v4.0', 'Release')]
	previous_analysis = params[:previous_analysis] || 'previous_ndepend'
	namespace :analysis do
		report_dir = "out/ndependout"
		directory report_dir
		desc 'Run NDepend analysis; see out/NDependOut/**/NDependReport.html'
		task :ndepend => [report_dir, :compile] do |nd|
			raise ArgumentError, "Project file not found at #{project_file}" unless File.exist? project_file
			project_file = File.expand_path(project_file).gsub('/','\\')
			out_dir = File.expand_path('out/ndependout/').gsub('/','\\')
			compare_with = '/AnalysisResultToCompareWith ' + File.expand_path("#{previous_analysis}/VisualNDepend.bin").gsub('/','\\') if File.exist?("#{previous_analysis}/VisualNDepend.bin")
			ndepend_in_dirs = in_dirs.map{|d| File.expand_path(d)}.join(' ').gsub('/','\\')
			cmd = CommandLine.new("#{third_party_path}/ndepend-v3/ndepend.console.exe", "#{project_file} /outdir #{out_dir} /concurrent #{compare_with} /indirs #{ndepend_in_dirs}", logger: @log, expected_outcodes:[0,1])
			stdout = cmd.run
			puts stdout
		end

		task :publish do
			xml = Pathname.new('out/ndependout/CQLRuleCriticalResult.xml').read
			doc = Nokogiri::XML xml
			violations = doc.css('RuleCritical').length
			puts "##teamcity[buildStatisticValue key='NDepend-Critical-Rule-Violations' value='#{violations}']"
		end
		task :default => [:ndepend, :publish]
	end
	desc 'Run all static analysis'
	task :analyse => 'analysis:default'
end
